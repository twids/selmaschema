import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { useAuth } from "../auth/AuthContext";
import { apiGet, apiPut } from "../api/client";
import type { ParentNamesDto } from "../api/types";

/* ---------- context shape ---------- */

interface ConfigContextValue {
  parentNames: ParentNamesDto;
  loading: boolean;
  updateParentNames: (parentA: string, parentB: string) => Promise<void>;
}

const ConfigContext = createContext<ConfigContextValue | undefined>(undefined);

/* ---------- defaults ---------- */

const DEFAULT_NAMES: ParentNamesDto = {
  parentAName: "Parent A",
  parentBName: "Parent B",
};

/* ---------- provider ---------- */

export function ConfigProvider({ children }: { children: React.ReactNode }) {
  const { authHeader } = useAuth();

  const [parentNames, setParentNames] = useState<ParentNamesDto>(DEFAULT_NAMES);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const data = await apiGet<ParentNamesDto>(
          "/api/config/parent-names",
          authHeader,
        );
        if (!cancelled) {
          setParentNames(data);
        }
      } catch {
        // Keep defaults on error
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [authHeader]);

  const updateParentNames = useCallback(
    async (parentA: string, parentB: string) => {
      const body: ParentNamesDto = {
        parentAName: parentA,
        parentBName: parentB,
      };
      const result = await apiPut<ParentNamesDto>(
        "/api/config/parent-names",
        body,
        authHeader,
      );
      setParentNames(result);
    },
    [authHeader],
  );

  const value: ConfigContextValue = {
    parentNames,
    loading,
    updateParentNames,
  };

  return (
    <ConfigContext.Provider value={value}>{children}</ConfigContext.Provider>
  );
}

/* ---------- hook ---------- */

export function useConfig(): ConfigContextValue {
  const ctx = useContext(ConfigContext);
  if (!ctx) {
    throw new Error("useConfig must be used within ConfigProvider");
  }
  return ctx;
}
