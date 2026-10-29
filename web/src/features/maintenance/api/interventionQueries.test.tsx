import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import type { ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { interventionKeys } from "./interventionKeys";
import { useDeleteIntervention } from "./interventionQueries";

const api = vi.hoisted(() => ({ listInterventions: vi.fn(), addIntervention: vi.fn(), deleteIntervention: vi.fn() }));
vi.mock("./interventionsApi", () => api);

function problem(status: number, code: string) {
  const response = { status, data: { code, title: "Erreur" } } as AxiosResponse;
  return new AxiosError("Erreur", "ERR_BAD_REQUEST", {} as InternalAxiosRequestConfig, null, response);
}

function setup() {
  const client = new QueryClient({ defaultOptions: { mutations: { retry: false } } });
  const invalidate = vi.spyOn(client, "invalidateQueries");
  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  );
  const { result } = renderHook(() => useDeleteIntervention("v1"), { wrapper });
  return { result, invalidate };
}

describe("useDeleteIntervention", () => {
  beforeEach(() => vi.clearAllMocks());

  it("rafraîchit le carnet sur 404 intervention.not_found", async () => {
    api.deleteIntervention.mockRejectedValue(problem(404, "intervention.not_found"));
    const { result, invalidate } = setup();

    await act(async () => {
      await result.current.mutateAsync("i1").catch(() => undefined);
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(invalidate).toHaveBeenCalledWith({ queryKey: interventionKeys.list("v1") });
  });

  it("n'invalide rien sur une autre erreur", async () => {
    api.deleteIntervention.mockRejectedValue(problem(500, "server.error"));
    const { result, invalidate } = setup();

    await act(async () => {
      await result.current.mutateAsync("i1").catch(() => undefined);
    });

    expect(invalidate).not.toHaveBeenCalled();
  });
});
