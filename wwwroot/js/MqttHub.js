"use strict";

let connection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Trace)
    .withUrl("/mqtthub")
    .build();

connection.on("ReceiveMqtt", (value) => {
    console.log(value);
    document.getElementById("mqtt").innerHTML = value
});


async function start() {
    try {
        await connection.start();
        console.log("Mqtt connected")
    } catch {
        console.log(err);
        setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    await start();
});

start();