import { describe, expect, it } from "vitest";
import type { DashboardDeadline } from "../types/dashboard";
import { countDeadlines, sortDeadlines } from "./sortDeadlines";

const make = (id: string, status: DashboardDeadline["status"], dueDate: string | null): DashboardDeadline => ({
  id,
  vehicle: { id: "v", licensePlate: "AA-111-AA", internalNumber: null, typeId: null, typeName: null },
  deadlineType: { id: "t", name: "CT" },
  status,
  dueDate,
  dueKm: null,
  currentKm: null,
});

describe("sortDeadlines", () => {
  it("place les dépassées avant les proches, puis par date croissante, sans date en dernier", () => {
    const input = [
      make("soon-late", "DueSoon", "2026-12-01"),
      make("over-late", "Overdue", "2026-10-10"),
      make("soon-early", "DueSoon", "2026-11-20"),
      make("over-none", "Overdue", null),
      make("over-early", "Overdue", "2026-09-01"),
    ];
    expect(sortDeadlines(input).map((d) => d.id)).toEqual([
      "over-early",
      "over-late",
      "over-none",
      "soon-early",
      "soon-late",
    ]);
    expect(input[0].id).toBe("soon-late");
  });

  it("compte dépassées et proches", () => {
    const list = [make("a", "Overdue", null), make("b", "DueSoon", null), make("c", "DueSoon", null)];
    expect(countDeadlines(list)).toEqual({ overdue: 1, dueSoon: 2 });
  });
});
