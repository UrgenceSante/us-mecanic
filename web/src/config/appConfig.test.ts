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

  it("laisse l'authentification active par défaut ou pour une valeur inconnue", () => {
    expect(resolveConfig().authDisabled).toBe(false);
    expect(resolveConfig({ authDisabled: "" }).authDisabled).toBe(false);
    expect(resolveConfig({ authDisabled: "1" }).authDisabled).toBe(false);
    expect(resolveConfig({ authDisabled: "oui" }).authDisabled).toBe(false);
    expect(resolveConfig({ authDisabled: "false" }).authDisabled).toBe(false);
  });

  it("désactive l'authentification uniquement pour la valeur explicite true", () => {
    expect(resolveConfig({ authDisabled: "true" }).authDisabled).toBe(true);
    expect(resolveConfig({ authDisabled: true }).authDisabled).toBe(true);
  });
});
