export const interventionKeys = {
  all: ["interventions"] as const,
  list: (vehicleId: string) => ["interventions", "list", vehicleId] as const,
};
