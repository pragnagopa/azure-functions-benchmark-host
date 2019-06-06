import { AzureFunctionsRpcMessages as rpc } from '../azure-functions-language-worker-protobuf/rpc';
export declare function fromTypedData(typedData?: rpc.ITypedData, convertStringToJson?: boolean): string | Buffer | null | undefined;
export declare function fromRpcHttp(rpcHttp: rpc.IRpcHttp): {
    method: string | null | undefined;
    url: string;
    originalUrl: string;
    headers: Dict<string>;
    query: Dict<string>;
    params: Dict<string>;
    body: string | Buffer | null | undefined;
};
export declare function toRpcHttp(inputMessage: any): rpc.ITypedData;
export declare function toTypedData(inputObject: any): rpc.ITypedData;
export interface Dict<T> {
    [key: string]: T;
}
