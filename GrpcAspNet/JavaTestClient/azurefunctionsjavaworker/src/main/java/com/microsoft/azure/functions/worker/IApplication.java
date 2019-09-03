package com.microsoft.azure.functions.worker;

public interface IApplication {
    String getHost();
    int getPort();
    Integer getMaxMessageSize();
}
