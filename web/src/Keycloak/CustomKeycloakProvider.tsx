import { ReactKeycloakProvider } from "@react-keycloak/web";
import keycloak from "./Keycloak";
import LogoLoader from "../components/Utils/LogoLoader";
import packageJson from "../../package.json";
import type { ReactNode } from "react";
import { appConfig } from "../config/appConfig";

interface CustomKeycloakProviderProps {
  children: ReactNode;
}
export default function CustomKeycloakProvider(
  props: CustomKeycloakProviderProps
) {
  // Mode dev sans authentification (ADR-0001) : Keycloak n'est pas initialisé.
  if (appConfig.authDisabled) {
    return props.children;
  }

  return (
    <ReactKeycloakProvider
      authClient={keycloak}
      initOptions={{ onLoad: "check-sso" }}
      LoadingComponent={
        <LogoLoader
          LoadingText={`Authentification - v${packageJson.version}`}
        />
      }
      onEvent={(e, err) => {
        if (e === "onInitError") {
          console.error("Keycloak init error", err);
          keycloak.onReady?.();
        }
      }}
    >
      {props.children}
    </ReactKeycloakProvider>
  );
}
