import { describe, expect, it } from "vitest";
import { formatEuro } from "./formatEuro";
import { parseAmount } from "./parseAmount";

describe("parseAmount", () => {
  it.each([
    ["", null],
    ["  ", null],
    ["0", 0],
    ["12,5", 12.5],
    ["12.50", 12.5],
    ["1 234,56", 1234.56],
    ["1 234,56", 1234.56],
    ["9999999999,99", 9_999_999_999.99],
  ])("accepte %j", (input, value) => {
    expect(parseAmount(input)).toEqual({ value, error: null });
  });

  it.each([
    ["-5", "Le montant ne peut pas être négatif."],
    ["1,234", "Le montant est limité à 2 décimales."],
    ["abc", "Montant invalide."],
    ["1,2,3", "Montant invalide."],
    [",", "Montant invalide."],
    ["10000000000", "Le montant dépasse la valeur maximale autorisée."],
  ])("refuse %j", (input, error) => {
    expect(parseAmount(input)).toEqual({ value: null, error });
  });
});

describe("formatEuro", () => {
  it("formate en euros avec 2 décimales", () => {
    expect(formatEuro(1234.5).replace(/\s/g, " ")).toBe("1 234,50 €");
    expect(formatEuro(0).replace(/\s/g, " ")).toBe("0,00 €");
  });
});
