import { AzureFunctionsRpcMessages as rpc } from '../azure-functions-language-worker-protobuf/rpc';
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
export declare class WorkerChannel implements IWorkerChannel {
    private _eventStream;
    private _workerId;
    constructor(workerId: string, eventStream: IEventStream);
    /**
     * Host requests worker to invoke a Function
     * @param requestId gRPC message request id
     * @param msg gRPC message content
     */
    invocationRequest(requestId: string, msg: rpc.InvocationRequest): void;
    /**
     * Worker sends the host information identifying itself
     */
    startStream(requestId: string, msg: rpc.StartStream): void;
}
export {};
