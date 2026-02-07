import { describe, it, expect } from 'vitest';
import '@testing-library/jest-dom';

describe('Testing Environment Setup', () => {
  it('should have vitest globals available', () => {
    expect(describe).toBeDefined();
    expect(it).toBeDefined();
    expect(expect).toBeDefined();
  });

  it('should have jest-dom matchers available', () => {
    const element = document.createElement('div');
    element.textContent = 'Hello World';
    document.body.appendChild(element);
    
    expect(element).toBeInTheDocument();
    expect(element).toHaveTextContent('Hello World');
    
    document.body.removeChild(element);
  });

  it('should have jsdom environment configured', () => {
    expect(window).toBeDefined();
    expect(document).toBeDefined();
    expect(document.body).toBeDefined();
  });
});
