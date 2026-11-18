import { describe, expect, it } from "vitest";
import { deadlineGap, todayInParis } from "./deadlineGap";

const today = "2026-10-01";

describe("deadlineGap", () => {
  it("dit depuis combien de jours une échéance est dépassée par la date", () => {
    expect(deadlineGap({ dueDate: "2026-09-19", dueKm: null, currentKm: null, status: "Overdue" }, today)).toEqual({
      text: "dépassée depuis 12 j",
      overdueBy: ["date"],
    });
  });

  it("dit de combien de km une échéance est dépassée", () => {
    const gap = deadlineGap({ dueDate: null, dueKm: 50_000, currentKm: 51_200, status: "Overdue" }, today);
    expect(gap?.text.replace(/\s/g, " ")).toBe("dépassée de 1 200 km");
    expect(gap?.overdueBy).toEqual(["km"]);
  });

  it("considère le km atteint comme dépassé, comme l'API", () => {
    const gap = deadlineGap({ dueDate: null, dueKm: 50_000, currentKm: 50_000, status: "Overdue" }, today);
    expect(gap?.overdueBy).toEqual(["km"]);
  });

  it("cite les deux critères quand les deux sont dépassés", () => {
    const gap = deadlineGap({ dueDate: "2026-09-30", dueKm: 100, currentKm: 150, status: "Overdue" }, today);
    expect(gap?.text).toBe("dépassée depuis 1 j et de 50 km");
    expect(gap?.overdueBy).toEqual(["date", "km"]);
  });

  it("annonce le délai d'une échéance proche", () => {
    expect(deadlineGap({ dueDate: "2026-10-07", dueKm: null, currentKm: null, status: "DueSoon" }, today)).toEqual({
      text: "dans 6 j",
      overdueBy: [],
    });
    expect(deadlineGap({ dueDate: today, dueKm: null, currentKm: null, status: "DueSoon" }, today)?.text).toBe("aujourd'hui");
  });

  it("ignore le critère km sans km actuel", () => {
    expect(deadlineGap({ dueDate: null, dueKm: 50_000, currentKm: null, status: "UpToDate" }, today)).toBeNull();
  });

  it("ne donne rien pour une échéance close ou sans statut", () => {
    expect(deadlineGap({ dueDate: "2026-09-01", dueKm: null, currentKm: null, status: null }, today)).toBeNull();
  });

  it("calcule la date du jour à l'heure de Paris", () => {
    expect(todayInParis(new Date("2026-10-01T22:30:00Z"))).toBe("2026-10-02");
  });
});
