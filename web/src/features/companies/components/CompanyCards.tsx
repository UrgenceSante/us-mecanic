import BlockIcon from "@mui/icons-material/Block";
import EditIcon from "@mui/icons-material/Edit";
import { Chip, IconButton, Stack, Typography } from "@mui/material";
import ListCard from "../../../shared/components/ListCard";
import type { Company } from "../types/company";

interface Props {
  companies: Company[];
  deactivating: boolean;
  onRename: (company: Company) => void;
  onDeactivate: (company: Company) => void;
}

const touchTarget = { width: 44, height: 44 };

/** Rendu mobile : une société par carte. */
export default function CompanyCards({ companies, deactivating, onRename, onDeactivate }: Props) {
  return (
    <Stack gap={1} role="list" aria-label="Sociétés" padding={1}>
      {companies.map((company) => (
        <ListCard key={company.id}>
          <Stack gap={0.5}>
            <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
              <Typography fontWeight="bold">{company.name}</Typography>
              <Chip
                size="small"
                label={company.isActive ? "Active" : "Désactivée"}
                color={company.isActive ? "success" : "default"}
              />
            </Stack>
            <Stack direction="row" justifyContent="flex-end" flexWrap="nowrap">
              <IconButton sx={touchTarget} aria-label={`Renommer ${company.name}`} onClick={() => onRename(company)}>
                <EditIcon />
              </IconButton>
              {company.isActive && (
                <IconButton
                  sx={touchTarget}
                  aria-label={`Désactiver ${company.name}`}
                  disabled={deactivating}
                  onClick={() => onDeactivate(company)}
                >
                  <BlockIcon />
                </IconButton>
              )}
            </Stack>
          </Stack>
        </ListCard>
      ))}
      {companies.length === 0 && <Typography>Aucune société.</Typography>}
    </Stack>
  );
}
