import client from "../../api/client";

const getSubscriptions = async () => {
  try {
    const request = await client.get("api/Notifications/Subscriptions");
    return request.data;
  } catch {
    return [];
  }
};

export { getSubscriptions };
