import CameraAltIcon from "@mui/icons-material/CameraAlt";
import UploadFileIcon from "@mui/icons-material/UploadFile";
import { Button, Stack, Typography } from "@mui/material";
import { useRef, type ChangeEvent } from "react";
import useBelow from "../../../shared/hooks/useBelow";
import { ACCEPTED_TYPES, formatFileSize, validateDocumentFile } from "./documentFile";

interface DocumentFilePickerProps {
  file: File | null;
  /** Message à afficher sous le sélecteur (contrôle avant envoi ou champ manquant). */
  error: string | null;
  onChange: (file: File | null, error: string | null) => void;
}

/** Choix du fichier ; sur mobile, un second bouton ouvre directement l'appareil photo. */
export default function DocumentFilePicker({ file, error, onChange }: DocumentFilePickerProps) {
  const mobile = useBelow("sm");
  const fileInput = useRef<HTMLInputElement>(null);
  const cameraInput = useRef<HTMLInputElement>(null);

  const pick = (event: ChangeEvent<HTMLInputElement>) => {
    const picked = event.target.files?.[0] ?? null;
    event.target.value = "";
    if (!picked) return;
    const problem = validateDocumentFile(picked);
    onChange(problem ? null : picked, problem);
  };

  return (
    <Stack gap={1}>
      <Stack direction="row" gap={1} flexWrap="wrap">
        <Button variant="outlined" startIcon={<UploadFileIcon />} onClick={() => fileInput.current?.click()} sx={{ minHeight: 44 }}>
          Choisir un fichier
        </Button>
        {mobile && (
          <Button variant="outlined" startIcon={<CameraAltIcon />} onClick={() => cameraInput.current?.click()} sx={{ minHeight: 44 }}>
            Prendre une photo
          </Button>
        )}
      </Stack>
      <input ref={fileInput} type="file" hidden accept={ACCEPTED_TYPES} onChange={pick} data-testid="document-file-input" />
      {mobile && (
        <input ref={cameraInput} type="file" hidden accept={ACCEPTED_TYPES} capture="environment" onChange={pick} data-testid="document-camera-input" />
      )}
      {file && (
        <Typography variant="body2">
          {file.name} ({formatFileSize(file.size)})
        </Typography>
      )}
      {error && (
        <Typography variant="body2" color="error" role="alert">
          {error}
        </Typography>
      )}
    </Stack>
  );
}
