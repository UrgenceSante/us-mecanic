import { Box } from "@mui/material";
import { useKeycloak } from "@react-keycloak/web";
import type { ReactNode } from "react";
import { appConfig } from "../config/appConfig";

interface RequireAuthProps {
  children: ReactNode;
}

function RequireKeycloakAuth(props: RequireAuthProps) {
  const { keycloak } = useKeycloak();

  if (!keycloak.authenticated) {
    keycloak.login();
    return (
      <Box display={"flex"} alignItems={"center"} justifyContent={"center"}>
        En attente d'authentification
      </Box>
    );
  }

  return props.children;
}

export default function RequireAuth(props: RequireAuthProps) {
  // Mode dev sans authentification (ADR-0001) : aucune connexion Keycloak.
  if (appConfig.authDisabled) {
    return props.children;
  }
  return <RequireKeycloakAuth>{props.children}</RequireKeycloakAuth>;
}
