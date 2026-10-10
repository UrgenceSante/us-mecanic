import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";

// Sans les globals vitest, Testing Library ne démonte pas les rendus entre deux tests.
afterEach(() => cleanup());
