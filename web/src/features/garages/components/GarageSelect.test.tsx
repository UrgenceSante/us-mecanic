import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import GarageSelect from "./GarageSelect";

const api = vi.hoisted(() => ({
  listGarages: vi.fn(),
  createGarage: vi.fn(),
  renameGarage: vi.fn(),
  deactivateGarage: vi.fn(),
}));
vi.mock("../api/garagesApi", () => api);

function renderSelect() {
  const onChange = vi.fn();
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <GarageSelect value={null} onChange={onChange} />
    </QueryClientProvider>,
  );
  return onChange;
}

async function typeAndValidate(text: string) {
  const input = await screen.findByRole("combobox");
  await waitFor(() => expect(api.listGarages).toHaveBeenCalled());
  fireEvent.change(input, { target: { value: text } });
  fireEvent.keyDown(input, { key: "Enter" });
}

describe("GarageSelect", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listGarages.mockResolvedValue([{ id: "g1", name: "Garage Dupont", isActive: true }]);
  });

  it("propose les garages actifs et sélectionne un garage existant", async () => {
    const onChange = renderSelect();
    fireEvent.mouseDown(await screen.findByRole("combobox"));

    fireEvent.click(await screen.findByRole("option", { name: "Garage Dupont" }));

    expect(api.listGarages).toHaveBeenCalledWith(false);
    expect(onChange).toHaveBeenCalledWith("g1");
    expect(api.createGarage).not.toHaveBeenCalled();
  });

  it("crée un garage inconnu puis le sélectionne", async () => {
    api.createGarage.mockResolvedValue({ id: "g9", name: "Norauto", isActive: true });
    const onChange = renderSelect();

    await typeAndValidate(" Norauto ");

    await waitFor(() => expect(api.createGarage).toHaveBeenCalledWith("Norauto"));
    await waitFor(() => expect(onChange).toHaveBeenCalledWith("g9"));
  });

  it("sélectionne le garage existant quand le nom est connu, sans le recréer", async () => {
    const onChange = renderSelect();

    await typeAndValidate("garage dupont");

    await waitFor(() => expect(onChange).toHaveBeenCalledWith("g1"));
    expect(api.createGarage).not.toHaveBeenCalled();
  });

  it("sélectionne le garage existant en cas de conflit 409", async () => {
    api.createGarage.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "garage.name_already_exists" } },
    });
    api.listGarages.mockResolvedValueOnce([]).mockResolvedValue([{ id: "g5", name: "Norauto", isActive: true }]);
    const onChange = renderSelect();

    await typeAndValidate("Norauto");

    await waitFor(() => expect(onChange).toHaveBeenCalledWith("g5"));
  });

  it("affiche l'erreur traduite si la création échoue", async () => {
    api.createGarage.mockRejectedValue({
      isAxiosError: true,
      response: { status: 400, data: { title: "Bad", code: "garage.name_too_long" } },
    });
    const onChange = renderSelect();

    await typeAndValidate("Norauto");

    expect(await screen.findByText("Le nom du garage est trop long.")).toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalled();
  });
});
