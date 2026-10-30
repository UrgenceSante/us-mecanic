export const loanKeys = {
  all: ["loans"] as const,
  list: (vehicleId: string) => ["loans", "list", vehicleId] as const,
};
