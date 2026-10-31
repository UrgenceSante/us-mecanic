export const expenseKeys = {
  all: ["expenses"] as const,
  list: (vehicleId: string) => ["expenses", "list", vehicleId] as const,
};
