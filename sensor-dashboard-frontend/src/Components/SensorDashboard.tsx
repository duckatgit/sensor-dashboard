"use client";

import React, { useEffect, useState } from "react";
import { startConnection } from "../services/signalRService";
import { Line } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import zoomPlugin from "chartjs-plugin-zoom";

ChartJS.register(zoomPlugin);
ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

// Types
interface BatchStat {
  sensorId: string;
  average: number;
  min: number;
  max: number;
  count: number;
  timestamp: string;
}

interface Alert {
  sensorId?: string;
  value?: number;
  maxValue?: number;
  message?: string;
  timestamp: string;
}

interface Stats {
  Min: number;
  Max: number;
  Average: number;
  Timestamp: string;
}

const SensorDashboard: React.FC = () => {
  const [chartData, setChartData] = useState<number[]>([]);
  const [chartLabels, setChartLabels] = useState<string[]>([]);
  const [stats, setStats] = useState<Stats | null>(null);
//   const [alerts, setAlerts] = useState<string[]>([]);
  const [alert, setAlert] = useState<string>(""); // only latest alert

  useEffect(() => {
    startConnection().then((conn: any) => {
      conn.on("ReceiveBatchStats", (args: BatchStat[]) => {
        if (args && args.length > 0) {
          const stat = args[0];
          setStats({
            Min: stat.min,
            Max: stat.max,
            Average: stat.average,
            Timestamp: stat.timestamp,
          });
          setChartData(prev => [...prev.slice(-49), stat.average]);
          setChartLabels(prev => [
            ...prev.slice(-49),
            new Date(stat.timestamp).toLocaleTimeString(),
          ]);
        }
      });

//       conn.on("Alert", (alert: Alert | Alert[]) => {
//   const alertsArray = Array.isArray(alert) ? alert : [alert]; // normalize to array
//   alertsArray.forEach(a => {
//     const msg = a.message || (a.maxValue ? `Max value exceeded: ${a.maxValue}` : "Unknown alert");
//     const ts = a.timestamp || new Date().toISOString();
//     setAlerts(prev => [...prev.slice(-19), `${new Date(ts).toLocaleTimeString()}: ${msg}`]);
//   });
// });
conn.on("Alert", (incoming: Alert | Alert[]) => {
  const alertObj = Array.isArray(incoming) ? incoming[0] : incoming; // get first alert if array
  if (alertObj.maxValue) {
    // const msg = `Max value exceeded: ${alertObj.maxValue}`;
    const msg = `Max value exceeded: ${alertObj.maxValue?.toFixed(2)}`;
    setAlert(msg); // only latest alert
  } else if (alertObj.message) {
    setAlert(alertObj.message);
  }
});
      conn.onclose((err: any) => console.error("SignalR disconnected", err));
    });
  }, []);

  return (
    <div style={{ padding: 20, backgroundColor: '#f5f5f5', fontFamily: 'Arial, sans-serif' }}>
      <h1 style={{ color: '#4B0082', marginBottom: 20 }}>Sensor Dashboard</h1>
      <div style={{ display: 'flex', gap: 20 }}>
        {/* Live Chart */}
        <div style={{ flex: 2, backgroundColor: 'white', padding: 15, borderRadius: 5 }}>
          <h3 style={{ marginBottom: 10 }}>Live Sensor Chart</h3>
          <Line
  data={{
    labels: chartLabels,
   datasets: [
  {
    label: "Sensor Average",
    data: chartData,
    borderColor: "lime",
    backgroundColor: "rgba(0,255,0,0.2)",
    fill: true,
    tension: 0.4, 
    pointRadius: 5, // <- pointer visible
    pointHoverRadius: 7, // <- bigger on hover
    pointBackgroundColor: "yellow", // <- pointer color
    pointBorderColor: "green", // <- border of pointer
    pointStyle: 'circle', // optional, 'rect' or 'triangle' bhi ho sakta
  },
],

  }}
  options={{
    responsive: true,
    plugins: {
      legend: { position: "top" },
      tooltip: { mode: "index", intersect: false },
      zoom: {
        zoom: {
          wheel: { enabled: true },
          pinch: { enabled: true },
          mode: "x",
        },
        pan: {
          enabled: true,
          mode: "x",
        },
      },
    },
    scales: {
      x: { 
        ticks: { maxRotation: 0, autoSkip: true },
      },
      y: { beginAtZero: false },
    },
  }}
/>

        </div>

        {/* Stats Panel */}
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 10 }}>
          <div style={{ background: 'linear-gradient(45deg, #6A0DAD, #A100F2)', color: 'white', padding: 10, borderRadius: 5, textAlign: 'center' }}>
            <h4>Average Value</h4>
            <p style={{ fontSize: '1.5em', margin: 0 }}>{stats?.Average?.toFixed(2) || '46.21'}</p>
          </div>
          <div style={{ background: 'linear-gradient(45deg, #00CED1, #20B2AA)', color: 'white', padding: 10, borderRadius: 5, textAlign: 'center' }}>
            <h4>Max Value</h4>
            <p style={{ fontSize: '1.5em', margin: 0 }}>{stats?.Max?.toFixed(2) || '99'}</p>
          </div>
          <div style={{ background: 'linear-gradient(45deg, #FF4500, #FF6347)', color: 'white', padding: 10, borderRadius: 5, textAlign: 'center' }}>
            <h4>Min Value</h4>
            <p style={{ fontSize: '1.5em', margin: 0 }}>{stats?.Min?.toFixed(2) || '0'}</p>
          </div>
        </div>
      </div>

      {/* Alerts (optional, can be added below if needed) */}
      {alert && (
  <div style={{ marginTop: 20, padding: 10, backgroundColor: '#FFCCCC', borderRadius: 5 }}>
    <strong>Alert:</strong> {alert}
  </div>
)}
    </div>
  );
};

export default SensorDashboard;