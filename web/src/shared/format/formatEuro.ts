const euro = new Intl.NumberFormat("fr-FR", { style: "currency", currency: "EUR" });

/** Montant en euros, à la française (ex. « 1 234,50 € »). */
export function formatEuro(amount: number): string {
  return euro.format(amount);
}
