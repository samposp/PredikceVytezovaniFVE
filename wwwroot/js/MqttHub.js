"use strict";

document.addEventListener("DOMContentLoaded", () => {
    let connection = new signalR.HubConnectionBuilder()
        .configureLogging(signalR.LogLevel.Trace)
        .withUrl("/mqtthub")
        .build();

    function updateChartByInterval(chartId, timestamp, value, intervalMinutes = 15, maxPoints = 200) {
        if (!window.charts || !window.charts[chartId]) return;

        const chart = window.charts[chartId];
        const labels = chart.data.labels;
        const data = chart.data.datasets[0].data;

        const currentTime = new Date(timestamp);

        // round down to interval boundary
        const bucketTime = new Date(currentTime);
        bucketTime.setSeconds(0, 0);
        bucketTime.setMinutes(Math.floor(bucketTime.getMinutes() / intervalMinutes) * intervalMinutes);

        const bucketMs = bucketTime.getTime();
        const lastIndex = labels.length - 1;

        if (lastIndex >= 0) {
            const lastLabel = new Date(labels[lastIndex]);

            const lastBucket = new Date(lastLabel);
            lastBucket.setSeconds(0, 0);
            lastBucket.setMinutes(Math.floor(lastBucket.getMinutes() / intervalMinutes) * intervalMinutes);

            const lastBucketMs = lastBucket.getTime();

            if (lastBucketMs === bucketMs) {
                // same 15-minute bucket -> move/update last point
                labels[lastIndex] = currentTime;
                data[lastIndex] = value;
                chart.update("none");
                return;
            }
        }

        // new 15-minute bucket -> append new point
        labels.push(currentTime);
        data.push(value);

        if (labels.length > maxPoints) {
            labels.shift();
            data.shift();
        }

        chart.update("none");
    }
    function parseCzDateTime(dateStr, timeStr) {
        const [day, month, year] = dateStr.split('.').map(Number);
        const [hour, minute, second] = timeStr.split(':').map(Number);

        return new Date(year, month - 1, day, hour, minute, second || 0);
    }


    connection.on("ReceiveMqtt", (value) => {
        console.log(value)
        document.getElementById("batteryPercentage").innerHTML = value.soC
        document.getElementById("grid").innerHTML = -value.p_GRID
        document.getElementById("PVEnergy").innerHTML = value.p_PV
        document.getElementById("consumption").innerHTML = value.p_HOME
        document.getElementById("lastTime").innerHTML = value.date + " " + value.time
        document.getElementById("battery").innerHTML = value.p_BAT
        document.getElementById("prize").innerHTML = value.pricE_CZK
        document.getElementById("buy").innerHTML = value.buy
        document.getElementById("sell").innerHTML = value.sell
        document.getElementById("fve").innerHTML = value.pVenergy
        document.getElementById("toBat").innerHTML = value.toBAT
        document.getElementById("fromBat").innerHTML = value.fromBAT
        document.getElementById("date").innerHTML = value.date

        const timestamp = parseCzDateTime(value.date, value.time);

        updateChartByInterval("batteryGraph", timestamp, value.soC, 15);
        updateChartByInterval("consumptionGraph", timestamp, value.p_HOME, 15);
        updateChartByInterval("fveGraph", timestamp, value.p_PV, 15);
        updateChartByInterval("gridGraph", timestamp, -value.p_GRID, 15);
    });


    async function start() {
        try {
            await connection.start();
            console.log("Mqtt connected")
        } catch(err) {
            console.error(err);
            setTimeout(start, 5000);
        }
    }

    connection.onclose(async () => {
        await start();
    });
    start();
});