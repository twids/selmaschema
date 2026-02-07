import { useEffect, useState } from "react";
import {
  Box,
  IconButton,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import { useAuth } from "../auth/AuthContext";
import { getMagicLinks, type MagicLinkResponse } from "../api/admin";

interface MagicLinksTableProps {
  /** Increment to trigger a refetch. */
  refreshKey: number;
}

/** Table displaying active magic-link invitations. */
export default function MagicLinksTable({ refreshKey }: MagicLinksTableProps) {
  const { authHeader } = useAuth();
  const [links, setLinks] = useState<MagicLinkResponse[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    getMagicLinks(authHeader)
      .then((data) => {
        if (!cancelled) setLinks(data);
      })
      .catch(() => {
        if (!cancelled) setLinks([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [refreshKey, authHeader]);

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleDateString("sv-SE", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });

  const truncate = (text: string, max = 40) =>
    text.length > max ? `${text.slice(0, max)}…` : text;

  return (
    <Box data-testid="magic-links-table">
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>E-post</TableCell>
              <TableCell>Roll</TableCell>
              <TableCell>Visningsnamn</TableCell>
              <TableCell>Magisk länk</TableCell>
              <TableCell>Utgår</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading &&
              Array.from({ length: 3 }).map((_, i) => (
                <TableRow key={`skeleton-${i}`} data-testid="skeleton-row">
                  {Array.from({ length: 5 }).map((_, j) => (
                    <TableCell key={j}>
                      <Skeleton variant="text" />
                    </TableCell>
                  ))}
                </TableRow>
              ))}

            {!loading && links.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  <Typography variant="body2" color="text.secondary">
                    Inga aktiva magiska länkar
                  </Typography>
                </TableCell>
              </TableRow>
            )}

            {!loading &&
              links.map((link) => (
                <TableRow key={link.token}>
                  <TableCell>{link.email}</TableCell>
                  <TableCell>{link.role}</TableCell>
                  <TableCell>{link.displayName || "—"}</TableCell>
                  <TableCell>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
                      <Typography
                        variant="body2"
                        sx={{ fontFamily: "monospace" }}
                      >
                        {truncate(link.magicLink)}
                      </Typography>
                      <IconButton
                        aria-label="Kopiera länk"
                        size="small"
                        onClick={() =>
                          navigator.clipboard.writeText(link.magicLink)
                        }
                      >
                        <ContentCopyIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  </TableCell>
                  <TableCell>{formatDate(link.expiresAt)}</TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
