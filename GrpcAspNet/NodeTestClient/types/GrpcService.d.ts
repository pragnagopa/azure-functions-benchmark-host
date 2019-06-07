import * as jsonModule from '../azure-functions-language-worker-protobuf/rpc';
import rpc = jsonModule.AzureFunctionsRpcMessages;
export interface IEventStream {
    write(message: rpc.IStreamingMessage): any;
    on(event: 'data', listener: (message: rpc.StreamingMessage) => void): any;
    on(event: string, listener: Function): any;
    end(): void;
}
export declare function CreateGrpcEventStream(connection: string): IEventStream;
