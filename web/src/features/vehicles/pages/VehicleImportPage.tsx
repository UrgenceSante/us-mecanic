import PageTitle from "../../../shared/components/PageTitle";
import { Alert, Button, Paper, Stack, Typography } from "@mui/material";
import KeyboardArrowLeftIcon from "@mui/icons-material/KeyboardArrowLeft";
import UploadFileIcon from "@mui/icons-material/UploadFile";
import { useRef, useState } from "react";
import { Link } from "react-router";
import { problemMessage } from "../../../shared/api/problemMessage";
import { useImportVehicles } from "../api/vehicleQueries";
import ImportReportView from "../components/ImportReportView";

/**
 * Import d'un parc existant en deux temps : simulation (bilan ligne par ligne, rien n'est enregistré),
 * puis confirmation. Les véhicules sont rapprochés par immatriculation.
 */
export default function VehicleImportPage() {
  const fileInput = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const importMutation = useImportVehicles();
  const report = importMutation.data;
  const canConfirm = report?.dryRun === true && report.created + report.updated > 0;

  const run = (dryRun: boolean) => file && importMutation.mutate({ file, dryRun });

  const selectFile = (selected: File | null) => {
    setFile(selected);
    importMutation.reset();
  };

  return (
    <Stack gap={2} marginY={2}>
      <div>
        <Button component={Link} to="/vehicles" startIcon={<KeyboardArrowLeftIcon />}>
          Parc
        </Button>
      </div>
      <PageTitle>Importer des véhicules</PageTitle>

      <Paper sx={{ padding: 2 }}>
        <Typography variant="body2" component="div">
          Fichier CSV (export Excel accepté, séparateur « ; » ou « , »), avec une ligne d'en-tête.
          <ul>
            <li>
              Colonne obligatoire : <b>Immatriculation</b> (format AB-123-CD).
            </li>
            <li>
              Colonnes facultatives : <b>Marque</b>, <b>Modèle</b>, <b>Mise en circulation</b> (JJ/MM/AAAA),{" "}
              <b>VIN</b> (ou N° de série), <b>Indicatif</b> (ou N° interne), <b>Propriétaire</b> (nom d'une société active). Les autres colonnes sont ignorées.
            </li>
            <li>
              Un véhicule déjà connu est mis à jour ; une cellule vide ne remplace pas une valeur existante.
            </li>
          </ul>
        </Typography>
        <Stack direction="row" gap={2} alignItems="center" flexWrap="wrap">
          <Button variant="outlined" startIcon={<UploadFileIcon />} onClick={() => fileInput.current?.click()}>
            Choisir un fichier
            <input
              ref={fileInput}
              hidden
              type="file"
              accept=".csv,text/csv"
              onChange={(e) => selectFile(e.target.files?.[0] ?? null)}
            />
          </Button>
          <Typography variant="body2">{file?.name ?? "Aucun fichier sélectionné"}</Typography>
          <Stack flexGrow={1} />
          <Button disabled={!file || importMutation.isPending} onClick={() => run(true)}>
            Simuler
          </Button>
          <Button variant="contained" disabled={!canConfirm || importMutation.isPending} onClick={() => run(false)}>
            Confirmer l'import
          </Button>
        </Stack>
      </Paper>

      {importMutation.isError && <Alert severity="error">{problemMessage(importMutation.error)}</Alert>}
      {report && <ImportReportView report={report} />}
    </Stack>
  );
}
