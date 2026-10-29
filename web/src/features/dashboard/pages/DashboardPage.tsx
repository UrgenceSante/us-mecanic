import { Stack, Tab, Tabs } from "@mui/material";
import { useState } from "react";
import DashboardProblems from "../../issues/components/DashboardProblems";
import DashboardDeadlines from "../components/DashboardDeadlines";

type DashboardView = "deadlines" | "problems";

export default function DashboardPage() {
  const [view, setView] = useState<DashboardView>("deadlines");
  return (
    <Stack gap={2}>
      <Tabs value={view} onChange={(_, v: DashboardView) => setView(v)} aria-label="Vues du tableau de bord">
        <Tab value="deadlines" label="Échéances" />
        <Tab value="problems" label="Problèmes" />
      </Tabs>
      {view === "deadlines" ? <DashboardDeadlines /> : <DashboardProblems />}
    </Stack>
  );
}
