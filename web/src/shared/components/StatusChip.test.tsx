import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import DeadlineStatusChip from "../../features/vehicle-deadlines/components/DeadlineStatusChip";
import BlockingLevelChip from "../../features/issues/components/BlockingLevelChip";
import ProblemStatusChip from "../../features/issues/components/ProblemStatusChip";
import VehicleStatusChip from "../../features/vehicles/components/VehicleStatusChip";
import ActiveStatusChip from "./ActiveStatusChip";
import UrgencyBadge from "./UrgencyBadge";

const iconOf = (el: HTMLElement) => el.querySelector("svg")?.getAttribute("data-testid");

describe("StatusChip et ses déclinaisons", () => {
  it.each([
    ["Active", "En service", "CheckCircleIcon"],
    ["Immobilized", "Immobilisé", "BlockIcon"],
    ["Retired", "Sorti du parc", "ArchiveIcon"],
  ] as const)("véhicule %s : libellé, icône et aria-label", (status, label, icon) => {
    render(<VehicleStatusChip status={status} />);
    const chip = screen.getByRole("img", { name: `Statut : ${label}` });
    expect(chip).toHaveTextContent(label);
    expect(iconOf(chip)).toBe(icon);
  });

  it.each([
    ["UpToDate", "À jour", "CheckCircleIcon", "colorSuccess"],
    ["DueSoon", "Proche", "ScheduleIcon", "colorWarning"],
    ["Overdue", "Dépassée", "ErrorIcon", "colorError"],
  ] as const)("échéance %s", (status, label, icon, tone) => {
    render(<DeadlineStatusChip status={status} isClosed={false} />);
    const chip = screen.getByRole("img", { name: `Statut : ${label}` });
    expect(iconOf(chip)).toBe(icon);
    expect(chip.className).toContain(tone);
  });

  it("échéance sans statut ou close", () => {
    render(<DeadlineStatusChip status={null} isClosed />);
    expect(iconOf(screen.getByRole("img", { name: "Statut : Clôturée" }))).toBe("LockIcon");
  });

  it.each([
    ["Immobilizing", "Immobilisant", "BlockIcon", "colorError"],
    ["Unanalyzed", "Non analysé", "HelpOutlineIcon", "colorWarning"],
    ["OverdueAction", "Action en retard", "AlarmIcon", "colorWarning"],
  ] as const)("niveau de blocage %s", (level, label, icon, tone) => {
    render(<BlockingLevelChip level={level} />);
    const chip = screen.getByRole("img", { name: `Niveau : ${label}` });
    expect(iconOf(chip)).toBe(icon);
    expect(chip.className).toContain(tone);
  });

  it.each([
    ["Reported", "Signalé"],
    ["Analyzed", "Analysé"],
    ["Resolved", "Résolu"],
  ] as const)("statut de problème %s", (status, label) => {
    render(<ProblemStatusChip status={status} />);
    expect(screen.getByRole("img", { name: `Statut : ${label}` })).toBeInTheDocument();
  });

  it("Actif / Désactivé", () => {
    render(<ActiveStatusChip isActive={false} />);
    expect(screen.getByRole("img", { name: "Statut : Désactivé" })).toBeInTheDocument();
  });
});

describe("UrgencyBadge", () => {
  it("n'affiche rien sans alerte", () => {
    const { container } = render(<UrgencyBadge alerts={{ overdueDeadlines: 0, immobilizingProblem: false }} />);
    expect(container).toBeEmptyDOMElement();
  });

  it("affiche le problème immobilisant et le nombre d'échéances dépassées", () => {
    render(<UrgencyBadge alerts={{ overdueDeadlines: 2, immobilizingProblem: true }} />);
    const chip = screen.getByRole("img", { name: "Alerte : 2 échéances dépassées, problème immobilisant" });
    expect(chip).toHaveTextContent("2 éch. · immobilisant");
  });
});
