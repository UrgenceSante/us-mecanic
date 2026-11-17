import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import SubscriptionContainer from "./SubscriptionContainer";

const http = vi.hoisted(() => ({ get: vi.fn() }));
vi.mock("../../api/client", () => ({ default: http }));

describe("SubscriptionContainer", () => {
  beforeEach(() => vi.resetAllMocks());

  it("affiche une alerte d'erreur si l'API répond 500", async () => {
    http.get.mockRejectedValue(new Error("Request failed with status code 500"));
    render(<SubscriptionContainer />);
    expect(await screen.findByRole("alert")).toHaveTextContent("Impossible de charger les abonnements.");
    expect(screen.queryByText("Aucun abonnement.")).toBeNull();
  });

  it("affiche l'état vide", async () => {
    http.get.mockResolvedValue({ data: [] });
    render(<SubscriptionContainer />);
    expect(await screen.findByText("Aucun abonnement.")).toBeInTheDocument();
  });
});
