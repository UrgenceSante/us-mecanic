import { AxiosError, AxiosHeaders } from "axios";
import { describe, expect, it } from "vitest";
import { problemMessage } from "./problemMessage";

function axiosError(data?: object, status = 409) {
  const response = data && {
    data,
    status,
    statusText: "",
    headers: {},
    config: { headers: new AxiosHeaders() },
  };
  return new AxiosError("Request failed", "ERR", undefined, undefined, response);
}

describe("problemMessage", () => {
  it("retourne le titre ProblemDetails de l'API", () => {
    expect(problemMessage(axiosError({ title: "Immatriculation en double" }))).toBe(
      "Immatriculation en double",
    );
  });

  it("signale une API injoignable quand il n'y a pas de réponse", () => {
    expect(problemMessage(axiosError())).toBe("API injoignable.");
  });

  it("retombe sur le message par défaut ou celui de l'erreur", () => {
    expect(problemMessage(axiosError({}), "Échec")).toBe("Échec");
    expect(problemMessage(new Error("boom"))).toBe("boom");
  });
});
