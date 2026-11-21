import BlockIcon from "@mui/icons-material/Block";
import EditIcon from "@mui/icons-material/Edit";
import { IconButton, Stack, Typography } from "@mui/material";
import ActiveStatusChip from "../../../shared/components/ActiveStatusChip";
import ListCard from "../../../shared/components/ListCard";
import type { DocumentCategory } from "../types/documentCategory";

interface Props {
  categories: DocumentCategory[];
  deactivating: boolean;
  onRename: (category: DocumentCategory) => void;
  onDeactivate: (category: DocumentCategory) => void;
}

const touchTarget = { width: 44, height: 44 };

/** Rendu mobile : une catégorie par carte. */
export default function DocumentCategoryCards({ categories, deactivating, onRename, onDeactivate }: Props) {
  return (
    <Stack gap={1} role="list" aria-label="Catégories de document" padding={1}>
      {categories.map((category) => (
        <ListCard key={category.id}>
          <Stack gap={0.5}>
            <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
              <Typography fontWeight="bold">{category.name}</Typography>
              <ActiveStatusChip isActive={category.isActive} />
            </Stack>
            <Stack direction="row" justifyContent="flex-end" flexWrap="nowrap">
              <IconButton sx={touchTarget} aria-label={`Renommer ${category.name}`} onClick={() => onRename(category)}>
                <EditIcon />
              </IconButton>
              {category.isActive && (
                <IconButton
                  sx={touchTarget}
                  aria-label={`Désactiver ${category.name}`}
                  disabled={deactivating}
                  onClick={() => onDeactivate(category)}
                >
                  <BlockIcon />
                </IconButton>
              )}
            </Stack>
          </Stack>
        </ListCard>
      ))}
      {categories.length === 0 && <Typography>Aucune catégorie.</Typography>}
    </Stack>
  );
}
