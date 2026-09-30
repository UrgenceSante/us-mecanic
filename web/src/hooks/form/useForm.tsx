import { useState } from "react";

export default function useForm<T>(initialState: T) {
  type FormErrors = Partial<Record<keyof T, string>>;

  const [formData, setFormData] = useState<T>(initialState);
  const [formErrors, setFormErrors] = useState<FormErrors>({});

  const validate = () => {
    const errors: FormErrors = {};
    if (!formData) return;
    (Object.keys(formData) as (keyof T)[]).forEach((key) => {
      if (!formData[key]) errors[key] = "Le champ est vide";
    });
    setFormErrors(errors);
  };

  const updateField = <K extends keyof T>(fieldName: K, value: T[K]) => {
    setFormData((old) => ({ ...old, [fieldName]: value }));
  };

  const submit = () => {
    validate();
  };

  return { formData, updateField, submit, formErrors };
}
