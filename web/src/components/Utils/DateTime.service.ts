export const time = (d: string) => new Date(d).getTime();
export const getShortTimeString = (d: string) =>
  new Date(d).toLocaleTimeString("fr-FR", { timeStyle: "short" });

export const getShortDateString = (d: string) =>
  new Date(d).toLocaleDateString("fr-FR", { dateStyle: "short" });
