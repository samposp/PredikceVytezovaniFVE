"use strict";

let connection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Trace)
    .withUrl("/mqtthub")
    .build();

connection.on("ReceiveMqtt", (value) => {
    console.log(value);
    document.getElementById("text").value = value
});

connection.start();

connection.invoke("Subscribe");