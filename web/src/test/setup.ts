import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach, vi } from "vitest";

// Sans les globals vitest, Testing Library ne démonte pas les rendus entre deux tests.
afterEach(() => cleanup());

// En production, `FrenchLocalizationProvider` est à la racine (main.tsx) : les rendus de test l'emploient aussi.
vi.mock("@testing-library/react", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@testing-library/react")>();
  const { default: FrenchLocalizationProvider } = await import("../shared/dates/FrenchLocalizationProvider");
  return {
    ...actual,
    render: (ui: Parameters<typeof actual.render>[0], options?: Parameters<typeof actual.render>[1]) =>
      actual.render(ui, { wrapper: FrenchLocalizationProvider, ...options }),
  };
});
