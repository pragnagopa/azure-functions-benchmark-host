import { AzureFunctionsRpcMessages as rpc } from '../azure-functions-language-worker-protobuf/rpc';
import { CreateGrpcEventStream } from './GrpcService';
import { WorkerChannel } from './WorkerChannel';
import * as parseArgs from 'minimist';

export function startNodeWorker(args) {
    let { serverUri, workerId } = parseArgs(args.slice(2));
    console.log(args);
    console.log(serverUri, workerId);
    console.log(`Worker ${workerId} connecting on ${serverUri}`);
    
    let eventStream;
    try {
      eventStream = CreateGrpcEventStream(serverUri);
    } catch (exception) {
      exception.message = "Error creating GRPC event stream: " + exception.message;
      throw exception;
    }
  
    let workerChannel = new WorkerChannel(workerId, eventStream);
    console.log("created grpc event stream");
    eventStream.write({
      startStream: {
        workerId: workerId
      }
    });
  
    process.on('uncaughtException', err => {
      console.error(`Worker ${workerId} uncaught exception: `, err);
      process.exit(1);
    });
    process.on('exit', code => {
      console.log(`Worker ${workerId} exited with code ${code}`);
    });
  }