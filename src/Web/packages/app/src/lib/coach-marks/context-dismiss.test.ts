import { describe, it, expect } from "vitest";
import { selectActiveMark, isSequenceDone, sequenceProgress } from "@nocturne/coach";
import type { MarkState, SequenceConfig } from "@nocturne/coach";

const sequences: SequenceConfig = {
  first: {
    priority: 100,
    steps: ["first.a", "first.b"],
  },
  second: {
    priority: 50,
    steps: ["second.a"],
  },
};

function makeStates(entries: Record<string, MarkState["status"]>): Map<string, MarkState> {
  const map = new Map<string, MarkState>();
  for (const [key, status] of Object.entries(entries)) {
    map.set(key, {
      id: key,
      markKey: key,
      status,
      seenAt: null,
      completedAt: null,
    });
  }
  return map;
}

function makeRegistration(key: string) {
  return {
    key,
    step: 0,
    title: key,
    description: key,
    priority: 0,
    element: {} as HTMLElement,
  };
}

describe("quiet dismiss behavior", () => {
  it("dismissing all steps in a sequence makes it done", () => {
    const states = makeStates({
      "first.a": "dismissed",
      "first.b": "dismissed",
    });
    expect(isSequenceDone("first", sequences, states)).toBe(true);
  });

  it("dismissed sequence satisfies prerequisite for dependent sequences", () => {
    const seqWithPrereq: SequenceConfig = {
      ...sequences,
      second: { ...sequences.second, prerequisite: "first" },
    };
    const states = makeStates({
      "first.a": "dismissed",
      "first.b": "dismissed",
    });
    expect(isSequenceDone("first", seqWithPrereq, states)).toBe(true);
  });

  it("selectActiveMark skips dismissed sequences and finds next eligible", () => {
    const states = makeStates({
      "first.a": "dismissed",
      "first.b": "dismissed",
    });
    const registrations = [makeRegistration("second.a")];

    const result = selectActiveMark(states, registrations, sequences);
    expect(result).toEqual({ key: "second.a", step: 0 });
  });

  it("selectActiveMark returns null when all sequences are dismissed", () => {
    const states = makeStates({
      "first.a": "dismissed",
      "first.b": "dismissed",
      "second.a": "dismissed",
    });
    const registrations = [
      makeRegistration("first.a"),
      makeRegistration("second.a"),
    ];

    const result = selectActiveMark(states, registrations, sequences);
    expect(result).toBeNull();
  });

  it("sequenceProgress counts dismissed steps as completed", () => {
    const states = makeStates({
      "first.a": "dismissed",
      "first.b": "completed",
    });
    const progress = sequenceProgress("first", sequences, states);
    expect(progress).toEqual({ completed: 2, total: 2 });
  });
});
