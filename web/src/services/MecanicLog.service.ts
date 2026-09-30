import client from "../api/client";
import type { IMecanicLog } from "../components/MecanicLog/IMecanicLog";

const getMecanicLogs = async () => {
  const request = await client.get("api/MecanicLog");
  return request.data;
};

const getMecanicLogsById = async (id: number) => {
  const mecanicLogs = await getMecanicLogs();
  return mecanicLogs.find((log: IMecanicLog) => log.LogId === id);
};

export { getMecanicLogs, getMecanicLogsById };
