package com.microsoft.azure.functions.worker;

import java.util.logging.*;
import javax.annotation.*;

import org.apache.commons.cli.*;
import org.apache.commons.lang3.exception.ExceptionUtils;

/**
 * The entry point of the Java Language Worker. Every component could get the command line options from this singleton
 * Application instance, and typically that instance will be passed to your components as constructor arguments.
 */
public final class Application {

    public static void main(String[] args) {
        String[] args_array =  args[0].split(":");

        String workerId =  args[1];
        String requestId = "request_1";

//        FunctionRpcGrpcClient client = new FunctionRpcGrpcClient(app.getHost(), app.getPort());
        try (JavaWorkerClient client = new JavaWorkerClient(args_array)) {
            System.out.println("hello1");
            client.listen(workerId, requestId).get();
            System.out.println("hello2");
        } catch (Exception ex) {
            System.out.println(ex);
            System.exit(-1);
        }
    }

    public static String version() {
        String jarVersion = Application.class.getPackage().getImplementationVersion();
        return jarVersion != null && !jarVersion.isEmpty() ? jarVersion : "Unknown";
    }
}
