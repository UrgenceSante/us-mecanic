import { describe, expect, it } from "vitest";
import { defaultConfig, resolveConfig } from "./appConfig";

describe("resolveConfig", () => {
  it("retourne les valeurs par défaut sans source", () => {
    expect(resolveConfig()).toEqual(defaultConfig);
  });

  it("applique les sources dans l'ordre, la dernière l'emporte", () => {
    const config = resolveConfig(
      { envName: "dev", apiUrl: "https://build" },
      { apiUrl: "https://runtime" },
    );
    expect(config.envName).toBe("dev");
    expect(config.apiUrl).toBe("https://runtime");
  });

  it("ignore les valeurs vides ou non définies", () => {
    const config = resolveConfig({ apiUrl: "", keycloakRealm: undefined });
    expect(config.apiUrl).toBe(defaultConfig.apiUrl);
    expect(config.keycloakRealm).toBe(defaultConfig.keycloakRealm);
  });
});
