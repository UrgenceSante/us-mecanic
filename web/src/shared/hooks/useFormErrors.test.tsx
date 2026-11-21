import { act, fireEvent, render, renderHook, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import useFormErrors from "./useFormErrors";

describe("useFormErrors", () => {
  it("valide quand aucun message n'est fourni", () => {
    const { result } = renderHook(() => useFormErrors<"km" | "date">());
    let valid = false;
    act(() => {
      valid = result.current.validate({ km: false, date: null });
    });
    expect(valid).toBe(true);
    expect(result.current.errors).toEqual({});
  });

  it("retient les messages par champ et renvoie faux", () => {
    const { result } = renderHook(() => useFormErrors<"km" | "date">());
    let valid = true;
    act(() => {
      valid = result.current.validate({ km: "Indiquez le kilométrage", date: undefined });
    });
    expect(valid).toBe(false);
    expect(result.current.errors).toEqual({ km: "Indiquez le kilométrage" });
  });

  it("efface l'erreur d'un champ modifié", () => {
    const { result } = renderHook(() => useFormErrors<"km" | "date">());
    act(() => {
      result.current.validate({ km: "Indiquez le kilométrage", date: "Indiquez la date" });
    });
    act(() => result.current.clear("km"));
    expect(result.current.errors).toEqual({ date: "Indiquez la date" });
  });

  it("fournit error et helperText, avec l'aide par défaut sans erreur", () => {
    const { result } = renderHook(() => useFormErrors<"km">());
    expect(result.current.fieldProps("km", "Facultatif")).toEqual({ name: "km", error: false, helperText: "Facultatif" });
    act(() => {
      result.current.validate({ km: "Indiquez le kilométrage" });
    });
    expect(result.current.fieldProps("km", "Facultatif")).toEqual({
      name: "km",
      error: true,
      helperText: "Indiquez le kilométrage",
    });
  });

  it("donne le focus au premier champ en erreur et marque les champs invalides", () => {
    function Form() {
      const { validate, fieldProps } = useFormErrors<"km" | "date">();
      return (
        <form noValidate onSubmit={(e) => (e.preventDefault(), validate({ km: "Indiquez le kilométrage", date: "Indiquez la date" }))}>
          <input aria-label="km" aria-invalid={fieldProps("km").error} name="km" />
          <input aria-label="date" aria-invalid={fieldProps("date").error} name="date" />
          <button>Valider</button>
        </form>
      );
    }
    render(<Form />);
    fireEvent.click(screen.getByRole("button", { name: "Valider" }));
    expect(screen.getByLabelText("km")).toHaveFocus();
    expect(screen.getByLabelText("km")).toBeInvalid();
    expect(screen.getByLabelText("date")).toBeInvalid();
  });
});
