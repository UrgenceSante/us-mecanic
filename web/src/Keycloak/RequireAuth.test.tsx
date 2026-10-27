import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

vi.mock("../config/appConfig", () => ({
  appConfig: { authDisabled: true },
}));

import RequireAuth from "./RequireAuth";

describe("RequireAuth", () => {
  it("affiche ses enfants sans Keycloak quand l'authentification est désactivée", () => {
    render(
      <RequireAuth>
        <div>contenu protégé</div>
      </RequireAuth>,
    );
    expect(screen.getByText("contenu protégé")).toBeTruthy();
  });
});
