export const vehicleDeadlineKeys = {
  all: ["vehicle-deadlines"] as const,
  list: (vehicleId: string) => ["vehicle-deadlines", "list", vehicleId] as const,
};
