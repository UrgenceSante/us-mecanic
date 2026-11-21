import { describe, expect, it } from "vitest";
import { formatFileSize, MAX_DOCUMENT_BYTES, SIZE_ERROR, TYPE_ERROR, validateDocumentFile } from "./documentFile";

const file = (type: string, size: number) => new File([new Uint8Array(size)], "f", { type });

describe("validateDocumentFile", () => {
  it("accepte PDF, JPEG et PNG", () => {
    for (const type of ["application/pdf", "image/jpeg", "image/png"]) expect(validateDocumentFile(file(type, 10))).toBeNull();
  });
  it("refuse un autre format", () => {
    expect(validateDocumentFile(file("image/heic", 10))).toBe(TYPE_ERROR);
  });
  it("refuse au-delà de 10 Mo et accepte 10 Mo pile", () => {
    expect(validateDocumentFile(file("application/pdf", MAX_DOCUMENT_BYTES + 1))).toBe(SIZE_ERROR);
    expect(validateDocumentFile(file("application/pdf", MAX_DOCUMENT_BYTES))).toBeNull();
  });
  it("refuse un fichier vide", () => {
    expect(validateDocumentFile(file("application/pdf", 0))).toBe("Le fichier est vide.");
  });
});

describe("formatFileSize", () => {
  it("affiche des Ko puis des Mo", () => {
    expect(formatFileSize(2048)).toContain("2 Ko");
    expect(formatFileSize(3 * 1024 * 1024)).toContain("3 Mo");
  });
});
