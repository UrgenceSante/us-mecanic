import {
  // Alert,
  Box,
  Button,
  FormControlLabel,
  FormGroup,
  // FormHelperText,
  Switch,
  TextField,
} from "@mui/material";
// import { useState } from "react";
import ConcerningSelect from "./Select/ConcerningSelect";
import NatureSelect from "./Select/NatureSelect";
import ActionForm from "./ActionForm";
import { v4 as uuidv4 } from "uuid";

import usePostAnalyze from "../../../hooks/analyse/usePostAnalyze";
// import { useParams } from "react-router";
import ErrorHandler from "../../Utils/Error/ErrorHandler";
import SimpleCard from "../../Utils/Cards/SimpleCard";
// import { useKeycloak } from "@react-keycloak/web";
import useForm from "../../../hooks/form/useForm";
import type { IAnalyseForm } from "../IAnalyse";
import type { IActionForm } from "../IAction";

interface AnalyzeFormProps {
  onCancel?: () => void;
  analyze?: IAnalyseForm;
  mutationFn?: (analyse: IAnalyseForm) => Promise<void>;
}
export default function AnalyseForm(props: AnalyzeFormProps) {
  // const params = useParams();
  // const { keycloak } = useKeycloak();

  const form = useForm<IAnalyseForm>(
    props.analyze || {
      Actions: [],
      Analyze: "",
      ConcerningId: "",
      ImmobilizeVehicle: false,
      NatureId: "",
    }
  );

  // const [hasFormError, setHasFormError] = useState(false);

  // const logId = params.logId ? parseInt(params.logId) : -1;

  const defaultAction: IActionForm = {
    ActionTypeId: "",
    ActorId: "",
    comment: "",
    ConstraintId: "",
    Id: uuidv4(),
    DueDate: "",
  };

  // const isActionValid = (action: IActionForm) => {
  //   if (!action.ActionTypeId) return false;
  //   if (!action.ActorId) return false;
  //   if (!action.ConstraintId) return false;
  //   if (action.RequiresDate && !action.DueDate) return false;
  //   return true;
  // };

  // const isFormValid = (form: IAnalyseForm) => {
  //   if (
  //     hasInvalidAction ||
  //     !form.Concerning.Id ||
  //     !form.Nature.Id ||
  //     !form.Analyze
  //   )
  //     return false;
  //   else return true;
  // };

  const mutation = usePostAnalyze(props.mutationFn);

  const addAction = () =>
    form.updateField("Actions", [...form.formData.Actions, defaultAction]);

  const deleteAction = (id: string | number) =>
    form.updateField(
      "Actions",
      form.formData.Actions.filter((action) => action.Id !== id)
    );

  const updateAction = (id: string | number, field: string, value: unknown) => {
    const newActions = form.formData.Actions.map((action) =>
      action.Id === id ? { ...action, [field]: value } : action
    );
    form.updateField("Actions", newActions);
  };

  // const formatToApi = (formData: IAnalyseForm) => {
  //   let command = formData;
  // command.Actions = formData.Actions.map((action) => ({
  //   ...action,
  //   Id: -1,
  //   DueDate: action.DueDate || null,
  // }));
  //   return command;
  // };

  // const submitForm = () => {
  // if (!isFormValid(formData)) return setHasFormError(true);
  // const command = formatToApi(formData);
  // mutation.mutate(command);
  // };

  // const hasInvalidAction = formData.Actions.some(
  //   (action) => !isActionValid(action)
  // );

  return (
    <>
      <SimpleCard title="Analyse">
        <Box>
          <FormGroup sx={{ gap: 2 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={form.formData.ImmobilizeVehicle}
                  onChange={() =>
                    form.updateField(
                      "ImmobilizeVehicle",
                      !form.formData.ImmobilizeVehicle
                    )
                  }
                />
              }
              label="Immobilisation du véhicule nécessaire"
            />

            <Box sx={{ display: "flex", gap: 2, width: "100%" }}>
              <ConcerningSelect
                value={form.formData.ConcerningId}
                onChange={(e) =>
                  form.updateField("ConcerningId", e.target.value)
                }
                error={!!form.formErrors.ConcerningId}
              />
              <NatureSelect
                value={form.formData.NatureId}
                onChange={(e) => form.updateField("NatureId", e.target.value)}
                error={!!form.formErrors.NatureId}
              />
            </Box>

            <TextField
              multiline
              rows={4}
              label="Analyse"
              helperText={form.formErrors.Analyze || ""}
              error={!!form.formErrors.Analyze}
              value={form.formData.Analyze}
              onChange={(e) => form.updateField("Analyze", e.target.value)}
            />
          </FormGroup>
        </Box>
      </SimpleCard>

      <Box>
        <SimpleCard title="Actions">
          <ActionForm
            deleteAction={deleteAction}
            updateAction={updateAction}
            addAction={addAction}
            actions={form.formData.Actions}
            canAddAction={true}
          />
        </SimpleCard>
        <Box
          sx={{ display: "flex", gap: 2, alignItems: "center", marginTop: 2 }}
        >
          <Button
            disabled={mutation.isPending}
            onClick={form.submit}
            variant="contained"
          >
            Sauvegarder
          </Button>
          {props.onCancel && (
            <Button onClick={props.onCancel} variant="outlined" color="error">
              Annuler
            </Button>
          )}
        </Box>
        {mutation.error && <ErrorHandler error={mutation.error} />}
      </Box>
    </>
  );
}
