import { AzureFunctionsRpcMessages as rpc } from '../azure-functions-language-worker-protobuf/rpc';

export function fromTypedData(typedData?: rpc.ITypedData, convertStringToJson: boolean = true) {
    typedData = typedData || {};
    let str = typedData.string || typedData.json;
    if (str !== undefined) {
        if (convertStringToJson) {
            try {
                if (str != null) {
                    str = JSON.parse(str);
                }
            } catch (err) { }
        }
        return str;
    } else if (typedData.bytes) {
        return Buffer.from(<Buffer>typedData.bytes);
    }
}

export function fromRpcHttp(rpcHttp: rpc.IRpcHttp) {
    const httpContext = {
      method: rpcHttp.method,
      url: <string>rpcHttp.url,
      originalUrl: <string>rpcHttp.url,
      headers: <Dict<string>>rpcHttp.headers,
      query: <Dict<string>>rpcHttp.query,
      params: <Dict<string>>rpcHttp.params,
      body: fromTypedData(<rpc.ITypedData>rpcHttp.body)
    };
  
    return httpContext;
}

export function toRpcHttp(inputMessage): rpc.ITypedData {
    let httpMessage: rpc.IRpcHttp = inputMessage;
    httpMessage.headers = toRpcHttpHeaders(inputMessage.headers);
    let status = inputMessage.statusCode || inputMessage.status;
    httpMessage.statusCode = status && status.toString();
    httpMessage.body = toTypedData(inputMessage.body);
    return { http: httpMessage };
}

export function toTypedData(inputObject): rpc.ITypedData {
    if (typeof inputObject === 'string') {
        return { string: inputObject };
    } else if (Buffer.isBuffer(inputObject)) {
        return { bytes: inputObject };
    } else if (ArrayBuffer.isView(inputObject)) {
        let bytes = new Uint8Array(inputObject.buffer, inputObject.byteOffset, inputObject.byteLength)
        return { bytes: bytes };
    } else if (typeof inputObject === 'number') {
        if (Number.isInteger(inputObject)) {
            return { int: inputObject };
        } else {
            return { double: inputObject };
        }
    } else {
        return { json: JSON.stringify(inputObject) };
    }
}

function toRpcHttpHeaders(inputHeaders: rpc.ITypedData) {
    let rpcHttpHeaders: {[key: string]: string} = {};
    for (let key in inputHeaders) {
        if (inputHeaders[key] != null) {
        rpcHttpHeaders[key] = inputHeaders[key].toString();
        }
    }
    return rpcHttpHeaders;
}

export interface Dict<T> {
    [key: string]: T
  }