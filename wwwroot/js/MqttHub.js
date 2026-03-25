"use strict";

document.addEventListener("DOMContentLoaded", () => {
    let connection = new signalR.HubConnectionBuilder()
        .configureLogging(signalR.LogLevel.Trace)
        .withUrl("/mqtthub")
        .build();

    connection.on("ReceiveMqtt", (value) => {
        console.log(value)
        document.getElementById("batteryPercentage").innerHTML = value.soC
        document.getElementById("grid").innerHTML = -value.p_GRID
        document.getElementById("PVEnergy").innerHTML = value.p_PV
        document.getElementById("consumption").innerHTML = value.p_HOME
    });


    async function start() {
        try {
            await connection.start();
            console.log("Mqtt connected")
        } catch {
            console.error(err);
            setTimeout(start, 5000);
        }
    }

    connection.onclose(async () => {
        await start();
    });
    start();
});