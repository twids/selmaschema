import { describe, it, expect } from 'vitest';
import { theme } from './theme';

describe('Theme Configuration', () => {
  it('should have the correct primary color (blue for Parent A)', () => {
    expect(theme.palette.primary.main).toBe('#1976d2');
  });

  it('should have the correct secondary color (green for Parent B)', () => {
    expect(theme.palette.secondary.main).toBe('#388e3c');
  });

  it('should have a valid theme structure', () => {
    expect(theme).toBeDefined();
    expect(theme.palette).toBeDefined();
    expect(theme.typography).toBeDefined();
  });

  it('should have proper spacing configuration', () => {
    expect(theme.spacing).toBeDefined();
  });
});
