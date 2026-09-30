import { Box } from "@mui/material";
import { useKeycloak } from "@react-keycloak/web";
import type { ReactNode } from "react";

interface RequireAuthProps {
  children: ReactNode;
}

export default function RequireAuth(props: RequireAuthProps) {
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
