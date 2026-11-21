import DeleteIcon from "@mui/icons-material/Delete";
import DownloadIcon from "@mui/icons-material/Download";
import OpenInNewIcon from "@mui/icons-material/OpenInNew";
import { IconButton, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from "@mui/material";
import ActionsHeaderCell from "../../../shared/components/ActionsHeaderCell";
import ListCard from "../../../shared/components/ListCard";
import { formatParisDate } from "../../../shared/format/formatParisDate";
import useBelow from "../../../shared/hooks/useBelow";
import { documentDisplayName, type VehicleDocument } from "../types/document";
import { formatFileSize } from "./documentFile";

interface DocumentListProps {
  documents: VehicleDocument[];
  /** Nom de catégorie par identifiant. */
  categoryNames: Map<string, string>;
  onOpen: (document: VehicleDocument) => void;
  onDownload: (document: VehicleDocument) => void;
  onDelete: (document: VehicleDocument) => void;
}

const touchTarget = { width: 44, height: 44 };

function Actions({ document, onOpen, onDownload, onDelete }: Omit<DocumentListProps, "documents" | "categoryNames"> & { document: VehicleDocument }) {
  const name = documentDisplayName(document);
  return (
    <Stack direction="row" sx={{ whiteSpace: "nowrap" }}>
      <IconButton sx={touchTarget} aria-label={`Ouvrir ${name}`} onClick={() => onOpen(document)}>
        <OpenInNewIcon fontSize="small" />
      </IconButton>
      <IconButton sx={touchTarget} aria-label={`Télécharger ${name}`} onClick={() => onDownload(document)}>
        <DownloadIcon fontSize="small" />
      </IconButton>
      <IconButton sx={touchTarget} aria-label={`Supprimer ${name}`} onClick={() => onDelete(document)}>
        <DeleteIcon fontSize="small" />
      </IconButton>
    </Stack>
  );
}

export default function DocumentList({ documents, categoryNames, ...actions }: DocumentListProps) {
  const compact = useBelow("sm");
  const categoryOf = (document: VehicleDocument) => (document.categoryId && categoryNames.get(document.categoryId)) || "-";

  if (compact) {
    return (
      <Stack gap={1} role="list">
        {documents.map((document) => (
          <ListCard key={document.id}>
            <Stack direction="row" alignItems="flex-start" gap={1}>
              <Stack flexGrow={1} minWidth={0} gap={0.5}>
                <Typography fontWeight="bold" sx={{ overflowWrap: "anywhere" }}>
                  {documentDisplayName(document)}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {categoryOf(document)} · {formatParisDate(document.addedAt)} · {formatFileSize(document.sizeBytes)}
                </Typography>
              </Stack>
              <Actions document={document} {...actions} />
            </Stack>
          </ListCard>
        ))}
      </Stack>
    );
  }

  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Document</TableCell>
            <TableCell>Catégorie</TableCell>
            <TableCell>Ajouté le</TableCell>
            <TableCell align="right">Taille</TableCell>
            <ActionsHeaderCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {documents.map((document) => (
            <TableRow key={document.id}>
              <TableCell sx={{ overflowWrap: "anywhere" }}>{documentDisplayName(document)}</TableCell>
              <TableCell>{categoryOf(document)}</TableCell>
              <TableCell>{formatParisDate(document.addedAt)}</TableCell>
              <TableCell align="right">{formatFileSize(document.sizeBytes)}</TableCell>
              <TableCell align="right">
                <Actions document={document} {...actions} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
