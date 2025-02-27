"use strict";

let connection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Trace)
    .withUrl("/mqtthub")
    .build();

connection.on("ReceiveMqtt", (value) => {
    document.getElementById("batteryPercentage").innerHTML = value.batteryPercentage
    document.getElementById("batteryOutput").innerHTML = value.batteryOutput
    document.getElementById("PVEnergy").innerHTML = value.pvOutput
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