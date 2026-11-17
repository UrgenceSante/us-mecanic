import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import FrenchDateField from "./FrenchDateField";

describe("FrenchDateField", () => {
  it("affiche une date ISO en JJ/MM/AAAA", () => {
    render(<FrenchDateField label="Du" value="2026-03-07" onChange={() => {}} />);
    const sections = screen.getAllByRole("spinbutton").map((s) => s.textContent);
    expect(sections.join("")).toBe("07/03/2026".replace(/\//g, ""));
  });
});
