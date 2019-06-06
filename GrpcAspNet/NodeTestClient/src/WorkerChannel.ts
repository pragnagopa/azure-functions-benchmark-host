import { AzureFunctionsRpcMessages as rpc } from '../azure-functions-language-worker-protobuf/rpc';
import { fromRpcHttp, fromTypedData, toRpcHttp } from './Converters'
import { IEventStream } from './GrpcService';

/**
 * The worker channel should have a way to handle all incoming gRPC messages.
 * This includes all incoming StreamingMessage types (exclude *Response types and RpcLog type)
 */
interface IWorkerChannel {
  startStream(requestId: string, msg: rpc.StartStream): void;
  invocationRequest(requestId: string, msg: rpc.InvocationRequest): void;
}

/**
 * Initializes handlers for incoming gRPC messages on the client
 */
export class WorkerChannel implements IWorkerChannel {
  private _eventStream: IEventStream;
  private _workerId: string;

  constructor(workerId: string, eventStream: IEventStream) {
    this._workerId = workerId;
    this._eventStream = eventStream;

    // call the method with the matching 'event' name on this class, passing the requestId and event message
    eventStream.on('data', (msg) => {
      let event = <string>msg.content;
      let eventHandler = (<any>this)[event];
      if (eventHandler) {
        eventHandler.apply(this, [msg.requestId, msg[event]]);
      } else {
        console.error(`Worker ${workerId} had no handler for message '${event}'`)
      }
    });
    eventStream.on('error', function (err) {
      console.error(`Worker ${workerId} encountered event stream error: `, err);
      throw err;
    });

    // wrap event stream write to validate message correctness
    let oldWrite = eventStream.write;
    eventStream.write = function checkWrite(msg) {
        let msgError = rpc.StreamingMessage.verify(msg);
        if (msgError) {
          console.error(`Worker ${workerId} malformed message`, msgError);
          throw msgError;
        }
        oldWrite.apply(eventStream, arguments);
    }
  }

  /**
   * Host requests worker to invoke a Function
   * @param requestId gRPC message request id
   * @param msg gRPC message content
   */
  public invocationRequest(requestId: string, msg: rpc.InvocationRequest) {
    let { functionId, inputData, invocationId } = msg;
    let bindings = {};
    let functionInputs: any[] = [];
    for (let binding of inputData) {
      if (binding.data && binding.name) {
        let input: any;
        if (binding.data && binding.data.http) {
          input = fromRpcHttp(binding.data.http);
        } else {
          input = fromTypedData(binding.data);
        }
        bindings[binding.name] = input;
        functionInputs.push(input);
      }
    }

    // Code that hard code loads a function and executes it.
    let myFunction = require('./functionapp/functionCode.js');
    myFunction(functionInputs).then((returnResult) => {
        let httpResponse = toRpcHttp(returnResult);

        let response: rpc.IInvocationResponse = {
            invocationId: msg.invocationId,
            result: "Good",
            returnValue: httpResponse
        };

        this._eventStream.write({
            requestId: requestId,
            invocationResponse: response
        });
    }).catch((err) => {
        console.log(err);
    });
  }

  /**
   * Worker sends the host information identifying itself
   */ 
  public startStream(requestId: string, msg: rpc.StartStream): void {
    // Not yet implemented
  }
}
