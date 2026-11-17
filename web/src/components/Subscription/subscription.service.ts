import client from "../../api/client";

const getSubscriptions = async () => {
  const request = await client.get("api/Notifications/Subscriptions");
  return request.data;
};

export { getSubscriptions };
