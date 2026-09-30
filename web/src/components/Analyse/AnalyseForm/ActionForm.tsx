import { Box, Button, IconButton, TextField } from "@mui/material";
import type { IActionForm } from "../IAction";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import ActionSelect from "./Select/ActionSelect";
import ActorSelect from "./Select/ActorSelect";
import ConstraintSelect from "./Select/ConstraintSelect";
import CustomDatePicker from "../../Utils/CustomDatePicker";

interface ActionFormProps {
  actions: IActionForm[];
  addAction: () => void;
  deleteAction: (id: string) => void;
  updateAction: (id: string, field: string, value: unknown) => void;
  canAddAction?: boolean;
}

export default function ActionForm(props: ActionFormProps) {
  // Pour l'affichage - Permet d'aligner les colonnes
  const hasDueDate = props.actions.some((action) => action.RequiresDate);
  return (
    <Box>
      <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
        {props.actions.map((action) => (
          <Box
            key={action.Id}
            sx={{
              display: "flex",
              gap: 2,
              alignItems: "center",
              width: "100%",
              justifyContent: "space-between",
            }}
          >
            <ActionSelect
              onChange={(e) =>
                props.updateAction(action.Id, "ActionTypeId", e.target.value)
              }
              value={action.ActionTypeId.toString()}
            />

            <ActorSelect
              onChange={(e) =>
                props.updateAction(action.Id, "ActorId", e.target.value)
              }
              value={action.ActorId.toString()}
            />

            <ConstraintSelect
              onChange={(e) =>
                props.updateAction(action.Id, "ConstraintId", e.target.value)
              }
              value={action.ConstraintId.toString()}
              label="Contrainte"
            />
            {action.RequiresDate ? (
              <CustomDatePicker
                sx={{ flex: 1 }}
                onChange={(date) =>
                  props.updateAction(action.Id, "DueDate", date?.toISOString())
                }
                value={new Date(action.DueDate || "")}
              />
            ) : (
              <Box sx={{ display: hasDueDate ? "block" : "none" }} flex={1} />
            )}

            <TextField
              sx={{ flex: 2 }}
              label="Commentaire"
              size="small"
              value={action.comment}
              onChange={(e) =>
                props.updateAction(action.Id, "comment", e.target.value)
              }
            />

            <IconButton
              onClick={() => props.deleteAction(action.Id)}
              color="error"
              size="small"
            >
              <DeleteIcon />
            </IconButton>
          </Box>
        ))}
      </Box>

      <Button
        sx={{ marginTop: 2 }}
        startIcon={<AddIcon />}
        onClick={props.addAction}
        disabled={!props.canAddAction}
      >
        Ajouter une action
      </Button>
    </Box>
  );
}
