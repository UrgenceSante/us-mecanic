import type { ProblemReferenceConfig, ProblemReferenceKind } from "../types/problemReference";

export const problemReferenceConfigs: ProblemReferenceConfig[] = [
  {
    kind: "nature",
    route: "api/problem-natures",
    errorPrefix: "problem_nature",
    title: "Natures",
    singular: "nature",
    definite: "la nature",
    feminine: true,
  },
  {
    kind: "element",
    route: "api/problem-elements",
    errorPrefix: "problem_element",
    title: "Éléments",
    singular: "élément",
    definite: "l'élément",
    feminine: false,
  },
  {
    kind: "actor",
    route: "api/problem-actors",
    errorPrefix: "problem_actor",
    title: "Acteurs",
    singular: "acteur",
    definite: "l'acteur",
    feminine: false,
  },
  {
    kind: "actionType",
    route: "api/problem-action-types",
    errorPrefix: "problem_action_type",
    title: "Types d'action",
    singular: "type d'action",
    definite: "le type d'action",
    feminine: false,
  },
  {
    kind: "constraint",
    route: "api/problem-constraints",
    errorPrefix: "problem_constraint",
    title: "Contraintes",
    singular: "contrainte",
    definite: "la contrainte",
    feminine: true,
  },
];

export function problemReferenceConfig(kind: ProblemReferenceKind): ProblemReferenceConfig {
  const config = problemReferenceConfigs.find((c) => c.kind === kind);
  if (!config) throw new Error(`Référentiel inconnu : ${kind}`);
  return config;
}
